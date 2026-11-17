using System.Linq;
using GameOff.EditorTools;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// Checks the generated pod scene is playable in principle: a player exists, every station exists once,
    /// and each station can actually be reached and targeted from its stand point.
    /// </summary>
    public class DronePodSceneTests
    {
        private const float EyeHeight = 1.6f;

        [OneTimeSetUp]
        public void OpenScene()
        {
            EditorSceneManager.OpenScene(PodLevelBuilder.ScenePath, OpenSceneMode.Single);
            Physics.SyncTransforms();
        }

        [Test]
        public void SceneHasOnePlayerWithCameraAndInteractor()
        {
            FirstPersonController[] players = Object.FindObjectsByType<FirstPersonController>();
            Assert.AreEqual(1, players.Length, "expected exactly one player");

            GameObject player = players[0].gameObject;
            Assert.IsNotNull(player.GetComponent<CharacterController>());
            Assert.IsNotNull(player.GetComponent<PlayerInteractor>());

            Camera camera = player.GetComponentInChildren<Camera>();
            Assert.IsNotNull(camera, "player has no camera");
            Assert.AreEqual("MainCamera", camera.tag);
        }

        [Test]
        public void SceneHasEachStationExactlyOnce()
        {
            Station[] stations = Object.FindObjectsByType<Station>();
            StationType[] expected = (StationType[])System.Enum.GetValues(typeof(StationType));

            CollectionAssert.AreEquivalent(expected, stations.Select(s => s.Type).ToArray());
        }

        [Test]
        public void PlayerStartsOnTheDeckInFreeSpace()
        {
            var controller = Object.FindAnyObjectByType<CharacterController>();
            AssertStandable(controller.transform.position, controller, "player start");
        }

        [Test]
        public void EveryStationHasAStandPointOnTheDeckInFreeSpace()
        {
            var controller = Object.FindAnyObjectByType<CharacterController>();

            foreach (Station station in Object.FindObjectsByType<Station>())
            {
                Assert.IsNotNull(station.StandPoint, station.Type + " has no stand point");
                AssertStandable(station.StandPoint.position, controller, station.Type + " stand point");
            }
        }

        [Test]
        public void EveryStationCanBeTargetedFromItsStandPoint()
        {
            foreach (Station station in Object.FindObjectsByType<Station>())
            {
                Assert.IsNotNull(station.FocusPoint, station.Type + " has no focus point");

                Vector3 eye = station.StandPoint.position + Vector3.up * EyeHeight;
                Vector3 direction = (station.FocusPoint.position - eye).normalized;

                bool hitSomething = Physics.Raycast(eye, direction, out RaycastHit hit, PlayerInteractor.DefaultRange,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

                Assert.IsTrue(hitSomething, station.Type + ": nothing within interaction range when looking at the focus point");
                Assert.AreSame(station, hit.collider.GetComponentInParent<Station>(),
                    station.Type + ": view from the stand point is blocked by " + hit.collider.name);
            }
        }

        [Test]
        public void PlayerCanWalkTheLengthOfThePod()
        {
            // Sweep the player's capsule along the centre aisle from the quarters to the cockpit.
            var controller = Object.FindAnyObjectByType<CharacterController>();
            const float clearance = 0.3f; // start the sweep above the deck so it only reports walls and furniture

            Vector3 start = new Vector3(0f, 0f, -5.2f);
            Vector3 bottom = start + Vector3.up * (controller.radius + clearance);
            Vector3 top = start + Vector3.up * (controller.height - controller.radius);
            const float distance = 9.6f; // to z = 4.4, just past the cockpit bulkhead doorway is checked below

            bool blocked = Physics.CapsuleCast(bottom, top, controller.radius, Vector3.forward, out RaycastHit hit, distance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            Assert.IsFalse(blocked, blocked ? "centre aisle blocked by " + hit.collider.name + " at z=" + hit.point.z.ToString("0.00") : string.Empty);
        }

        private static void AssertStandable(Vector3 position, CharacterController controller, string label)
        {
            bool overDeck = Physics.Raycast(position + Vector3.up * 0.5f, Vector3.down, out RaycastHit ground, 1f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            Assert.IsTrue(overDeck, label + " is not above any floor");
            Assert.AreEqual("Deck", ground.collider.name, label + " is standing on " + ground.collider.name);

            // Lift the capsule slightly so resting on the deck does not count as an overlap.
            Vector3 feet = new Vector3(position.x, ground.point.y + 0.05f, position.z);
            Vector3 bottom = feet + Vector3.up * controller.radius;
            Vector3 top = feet + Vector3.up * (controller.height - controller.radius);
            Collider[] overlaps = Physics.OverlapCapsule(bottom, top, controller.radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            Assert.IsEmpty(overlaps.Select(c => c.name).ToArray(), label + " overlaps scene geometry");
        }
    }
}

using System.Collections.Generic;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.XR;
using Grate.Gestures;

namespace Grate.Modules.Movement
{
    internal class Frozone : GrateModule
    {
        private static GameObject IcePrefab;

        private static readonly Dictionary<bool, List<GameObject>> frozonicPlatforms = new();
        private static readonly Dictionary<bool, List<float>> frozonicTimes = new();
        private static readonly Dictionary<bool, int> platformIndex = new();

        private const float lifetime = 2.5f;
        private const int maxPlatforms = 72;

        private bool leftGrab;
        private bool rightGrab;

        public override string GetDisplayName() => "Frozone";

        public override string Tutorial() =>
            "Hold grip to create ice platforms. They last ~2.5 seconds.";

        protected override void Start()
        {
            base.Start();

            // restore YOUR original prefab usage
            IcePrefab = Plugin.AssetBundle.LoadAsset<GameObject>("Ice");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            var left = GestureTracker.Instance.GetInputTracker("grip", XRNode.LeftHand);
            var right = GestureTracker.Instance.GetInputTracker("grip", XRNode.RightHand);

            left.OnPressed += OnActivate;
            left.OnReleased += OnDeactivate;

            right.OnPressed += OnActivate;
            right.OnReleased += OnDeactivate;
        }

        protected override void Cleanup()
        {
            var left = GestureTracker.Instance.GetInputTracker("grip", XRNode.LeftHand);
            var right = GestureTracker.Instance.GetInputTracker("grip", XRNode.RightHand);

            left.OnPressed -= OnActivate;
            left.OnReleased -= OnDeactivate;

            right.OnPressed -= OnActivate;
            right.OnReleased -= OnDeactivate;
        }

        private void OnActivate(InputTracker tracker)
        {
            if (tracker.node == XRNode.LeftHand) leftGrab = true;
            if (tracker.node == XRNode.RightHand) rightGrab = true;
        }

        private void OnDeactivate(InputTracker tracker)
        {
            if (tracker.node == XRNode.LeftHand) leftGrab = false;
            if (tracker.node == XRNode.RightHand) rightGrab = false;
        }

        private (Vector3 position, Quaternion rotation, Vector3 right) GetHand(bool left)
        {
            var hand = left
                ? GorillaTagger.Instance.leftHandTransform
                : GorillaTagger.Instance.rightHandTransform;

            return (hand.position, hand.rotation, hand.right);
        }

        private void FixedUpdate()
        {
            HandleFrozone(true);
            HandleFrozone(false);
        }

        public void HandleFrozone(bool left)
        {
            bool grip = left ? leftGrab : rightGrab;

            if (!frozonicPlatforms.TryGetValue(left, out var list))
            {
                list = new List<GameObject>();
                frozonicPlatforms[left] = list;
            }

            if (!frozonicTimes.TryGetValue(left, out var times))
            {
                times = new List<float>();
                frozonicTimes[left] = times;
            }

            platformIndex.TryGetValue(left, out int index);

            if (grip)
            {
                var hand = GetHand(left);

                GameObject platform;

                if (list.Count >= maxPlatforms)
                {
                    platform = list[index];
                }
                else
                {
                    platform = Object.Instantiate(IcePrefab);
                    platform.AddComponent<GorillaSurfaceOverride>().overrideIndex = 61;

                    list.Add(platform);
                    times.Add(Time.time);
                }

                platform.transform.position = hand.position;
                platform.transform.rotation = hand.rotation;

                if (index < times.Count)
                    times[index] = Time.time;

                platformIndex[left] = (index + 1) % maxPlatforms;
            }

            // 2.5 second lifetime cleanup
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (Time.time - times[i] > lifetime)
                {
                    Object.Destroy(list[i]);
                    list.RemoveAt(i);
                    times.RemoveAt(i);
                }
            }
        }
    }
}
using GorillaLocomotion;
using Grate.GUI;
using UnityEngine;

namespace Grate.Modules.Physics;

public class Freeze : GrateModule
{
    public static readonly string DisplayName = "Freeze";
    public static Freeze Instance;

    private Vector3 frozenPosition;
    private Quaternion frozenRotation;

    private void Awake()
    {
        Instance = this;
    }

    protected override void OnEnable()
    {
        if (!MenuController.Instance.Built) return;

        base.OnEnable();

        frozenPosition = GTPlayer.Instance.transform.position;
        frozenRotation = GTPlayer.Instance.transform.rotation;
    }

    private void LateUpdate()
    {
        return;

        // Teleport
        GTPlayer.Instance.TeleportTo(frozenPosition, frozenRotation);

        // Stop movement
        GTPlayer.Instance.currentVelocity = Vector3.zero;
        GorillaTagger.Instance.rigidbody.velocity = Vector3.zero;
        GorillaTagger.Instance.rigidbody.angularVelocity = Vector3.zero;
    }

    protected override void Cleanup()
    {
        // nothing needed
    }

    public override string GetDisplayName()
    {
        return DisplayName;
    }

    public override string Tutorial()
    {
        return "Freezes you in place.\n!!! This Mod is currently disabled because it makes the Game unplayable !!!";
    }
}
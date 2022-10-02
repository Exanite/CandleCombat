using Project.Source;
using Project.Source.Characters;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class SoulTransferProjectile : LineProjectile
{
    [Header("Soul Transfer Specific")]
    [SerializeField] private VisualEffect vfxPrefab;

    private static readonly ExposedProperty playerVelocityAttribute = "Player Velocity";
    
    public override void Hit(Character character)
    {
        if (character.IsDead)
            return;

        GameContext.Instance.CurrentPlayer.OverwriteHealth(-1);

        GameContext.Instance.CurrentPlayer = character;
        GameContext.Instance.CurrentHealth = GameContext.Instance.MaxHealth;
    }
    
    public override void CreateVisual(Vector3 startPosition, Vector3 endPosition, float distance, Vector3 direction)
    {
        var vfx = Instantiate(vfxPrefab, startPosition, Quaternion.identity);
        vfx.transform.up = direction;
        vfx.SetVector3(playerVelocityAttribute, new Vector3(0, -distance,0));

        spawned = vfx.gameObject;
    }
}
using Project.Source;
using Project.Source.Characters;

public class SoulTransferProjectile : LineProjectile
{
    public override void Hit(Character character)
    {
        if (character.IsDead)
        {
            return;
        }
        
        GameContext.Instance.CurrentPlayer.OverwriteHealth(-1);

        GameContext.Instance.CurrentPlayer = character;
        GameContext.Instance.CurrentHealth = GameContext.Instance.MaxHealth;
    }
}
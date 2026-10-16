// Implemented by every turret so TurretUpgrade can push new stats when the level changes.
public interface ILevelable
{
    void ApplyLevel(int level); // 1-based: 1 = freshly built, 3 = max level
}

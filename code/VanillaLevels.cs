using GameData;

namespace GTFR
{
    public class VanillaLevels
    {
        private readonly LevelLayoutDataBlock[] layouts;

        public VanillaLevels (LevelLayoutDataBlock[] ingame)
        {
            var copies = new LevelLayoutDataBlock[ingame.Length];

            for (int i = 0; i < ingame.Length; i++)
            {
                var original = ingame[i];

                try
                {
                    var copy = GameDataBlockBase<LevelLayoutDataBlock>.CreateNewCopy (original);
                    copy.name = original.name;

                    copies[i] = copy;
                }

                catch (Exception)
                {
                    // EntryPoint.godhelpus.Log.LogError (e.ToString ());
                }
            }

            layouts = copies;
        }

        public LevelLayoutDataBlock OriginalLayoutOf (string name)
        {
            foreach (var layout in layouts)
            {
                if (layout != null)
                {
                    if (layout.name.Contains (name))
                    {
                        return layout;
                    }
                }
            }

            throw new ArgumentException ("Could not find original layout for level " + name);
        }
    }
}

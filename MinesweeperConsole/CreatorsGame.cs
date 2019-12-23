using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperConsole
{
    abstract class Creator
    {
        public abstract IGame Create();

    }
    class CreatorSingleGame : Creator
    {
        public override IGame Create()
        {
            return new SingleGame();
        }
    }
    class CreatorMultiGame : Creator
    {
        public override IGame Create()
        {
            return new MultiGame();
        }
    }
    class CreatorAIGame : Creator
    {
        public override IGame Create()
        {
            return new AIGame();
        }
    }
}

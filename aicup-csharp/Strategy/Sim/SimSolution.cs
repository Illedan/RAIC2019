namespace aicup2019.Strategy.Sim
{
    public class SimSolution
    {
        public double Score;
        public double BestScore = -100000000;
        public MyAction[] BestActions;
        public MyAction[] TempActions;

        public SimSolution()
        {
            BestActions = new MyAction[Const.Depth];
            TempActions = new MyAction[Const.Depth];
            for(var i = 0; i < Const.Depth; i++)
            {
                BestActions[i] = MyAction.Actions[Const.rnd.Next(0, MyAction.Actions.Count)];
                TempActions[i] = MyAction.Actions[Const.rnd.Next(0, MyAction.Actions.Count)];
            }
        }

        public void ResetBestScore()
        {
            BestScore = -100000000;
        }

        public void CloneBestIntoTemp()
        {
            for (var i = 0; i < Const.Depth; i++)
            {
                TempActions[i] = BestActions[i];
            }
        }

        public void Mutate()
        {
            var changed = false;
            while (!changed)
            {
                for (var i = 0; i < Const.Depth; i++)
                {
                    var prev = TempActions[i];
                    TempActions[i] = Const.rnd.NextDouble() < 0.3 ? MyAction.Actions[Const.rnd.Next(MyAction.Actions.Count)] : BestActions[i];
                    if (!changed && prev != TempActions[i])
                    {
                        changed = true;
                    }
                }
            }
        }

        public void Randomize()
        {
            for (var i = 0; i < Const.Depth; i++)
            {
                TempActions[i] = MyAction.Actions[Const.rnd.Next(MyAction.Actions.Count)];
            }
        }

        public void SetFromMoves(int id)
        {
            for (var i = 0; i < Const.Depth; i++)
            {
                TempActions[i] = MyAction.Actions[id];
            }
        }

        public void AfterRound()
        {
            if(Score > BestScore)
            {
                var temp = TempActions;
                BestScore = Score;
                TempActions = BestActions;
                BestActions = temp;
            }

            Score = 0;
        }
    }
}

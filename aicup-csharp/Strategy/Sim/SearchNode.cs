namespace aicup2019.Strategy.Sim
{
    public class SearchNode
    {
        public SearchNode(MyAction action)
        {
            Action = action;
        }
        public MyAction Action;
        public int Usages ;
        public double Score;
        public double AvreageScore;
        //TODO: Children?
        public void Update(double score)
        {
            Usages++;
            Score += score;
            AvreageScore = Score / Usages;
        }
    }
}

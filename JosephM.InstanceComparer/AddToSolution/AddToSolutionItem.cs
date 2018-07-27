namespace JosephM.InstanceComparer.AddToSolution
{
    public class AddToSolutionItem
    {
        public AddToSolutionItem(int componentType, string componentId)
        {
            ComponentType = componentType;
            ComponentId = componentId;
        }

        public int ComponentType { get; set; }
        public string ComponentId { get; set; }
    }
}

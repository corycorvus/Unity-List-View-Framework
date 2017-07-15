namespace ListView
{
    class CubeList : ListViewController<CubeItemData, CubeItem, int>
    {
        protected override void Setup()
        {
            base.Setup();
            for (int i = 0; i < data.Count; i++)
            {
                data[i].text = i + "";
            }
        }
    }
}
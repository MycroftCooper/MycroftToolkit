namespace GameAttribute {
    public class AttributeTest : AttributeManager {
        private const string OwnerName = "Owner";
        public override string Owner => OwnerName;

        public Attribute a = new Attribute("A");
        public Attribute b = new Attribute("B");
        public Attribute minC = new Attribute("MinC");
        public Attribute maxC = new Attribute("MaxC");
        public Attribute c = new Attribute("C");
    }
}
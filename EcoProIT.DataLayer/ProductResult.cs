
namespace EcoProIT.DataLayer
{
    using System;


    public partial class ProductResult :SimulationResult, IComparable<ProductResult>
    {
        private readonly SimulationResult Result = new SimulationResult();
        public string ProductType { get; set; }
        public string ModelNode { get; set; }
        public ulong Productid { get; set; }



        public int CompareTo(ProductResult other)
        {
            if (ProductType.Equals(other.ProductType) && ModelNode.Equals(other.ModelNode) && Productid == other.Productid && Start == other.Start)
                return 0;
            var i = System.String.Compare(ProductType, other.ProductType, System.StringComparison.Ordinal);
            if (i != 0)
                return i;
            i = System.String.Compare(ModelNode, other.ModelNode, System.StringComparison.Ordinal);
            if (i != 0)
                return i;
            i = (int) (Productid - other.Productid);
            if (i != 0)
                return i;
            return (int)(Start - other.Start);
        }

        public override bool Equals(object obj)
        {
            var other = obj as ProductResult;
            if (other != null)
                return (ProductType.Equals(other.ProductType) && ModelNode.Equals(other.ModelNode) &&
                        Productid == other.Productid && Start == other.Start);
            return false;
        }

        public override int GetHashCode()
        {
            return (ProductType + ModelNode + Productid + Start).GetHashCode();
        }

        public override string ToString()
        {
            return ProductType + " " + ModelNode + " " + Productid + " " + Start + " " + Total;
        }
    }
}

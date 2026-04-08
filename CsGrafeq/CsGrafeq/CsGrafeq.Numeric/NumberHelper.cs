using CsGrafeq.Numeric.Exact;

namespace CsGrafeq.Numeric;

public static class NumberHelper
{
    extension(double self)
    {
        public ExactNumber ToExact()
        {
            return ExactNumber.CreateFloat(self,true);
        }
    }
    extension(int self)
    {
        public ExactNumber ToExact()
        {
            return ExactNumber.CreateFloat(self,true);
        }
    }

    extension(Vec self)
    {
        public VectorExact ToExact()
        {
            return new VectorExact(self.X.ToExact(), self.Y.ToExact());
        }
    }
    
}
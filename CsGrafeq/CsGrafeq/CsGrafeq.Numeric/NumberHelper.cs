using CsGrafeq.Numeric.Exact;

namespace CsGrafeq.Numeric;

public static class NumberHelper
{
    extension(double self)
    {
        public ExactNumber ToExact()
        {
            return ExactNumber.CreateFloat(self);
        }
    }
    extension(int self)
    {
        public ExactNumber ToExact()
        {
            return ExactNumber.CreateFloat(self);
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
using CsGrafeq.Collections;

namespace CsGrafeq.Numeric.Exact;

public static class DecimalPart
{
    private const double Eps= 1e-8;
    public static BiDictionary<double, Rational> Decimal { get; } = new()
    {
        { 0.03225806451612903, new Rational(1, 1, 31) },
        { 0.03333333333333333, new Rational(1, 1, 30) },
        { 0.034482758620689655, new Rational(1, 1, 29) },
        { 0.03571428571428571, new Rational(1, 1, 28) },
        { 0.037037037037037035, new Rational(1, 1, 27) },
        { 0.038461538461538464, new Rational(1, 1, 26) },
        { 0.04, new Rational(1, 1, 25) },
        { 0.041666666666666664, new Rational(1, 1, 24) },
        { 0.043478260869565216, new Rational(1, 1, 23) },
        { 0.045454545454545456, new Rational(1, 1, 22) },
        { 0.047619047619047616, new Rational(1, 1, 21) },
        { 0.05, new Rational(1, 1, 20) },
        { 0.05263157894736842, new Rational(1, 1, 19) },
        { 0.05555555555555555, new Rational(1, 1, 18) },
        { 0.058823529411764705, new Rational(1, 1, 17) },
        { 0.0625, new Rational(1, 1, 16) },
        { 0.06451612903225806, new Rational(1, 2, 31) },
        { 0.06666666666666667, new Rational(1, 1, 15) },
        { 0.06896551724137931, new Rational(1, 2, 29) },
        { 0.07142857142857142, new Rational(1, 1, 14) },
        { 0.07407407407407407, new Rational(1, 2, 27) },
        { 0.07692307692307693, new Rational(1, 1, 13) },
        { 0.08, new Rational(1, 2, 25) },
        { 0.08333333333333333, new Rational(1, 1, 12) },
        { 0.08695652173913043, new Rational(1, 2, 23) },
        { 0.09090909090909091, new Rational(1, 1, 11) },
        { 0.09523809523809523, new Rational(1, 2, 21) },
        { 0.0967741935483871, new Rational(1, 3, 31) },
        { 0.1, new Rational(1, 1, 10) },
        { 0.10344827586206896, new Rational(1, 3, 29) },
        { 0.10526315789473684, new Rational(1, 2, 19) },
        { 0.10714285714285714, new Rational(1, 3, 28) },
        { 0.1111111111111111, new Rational(1, 1, 9) },
        { 0.11538461538461539, new Rational(1, 3, 26) },
        { 0.11764705882352941, new Rational(1, 2, 17) },
        { 0.12, new Rational(1, 3, 25) },
        { 0.125, new Rational(1, 1, 8) },
        { 0.12903225806451613, new Rational(1, 4, 31) },
        { 0.13043478260869565, new Rational(1, 3, 23) },
        { 0.13333333333333333, new Rational(1, 2, 15) },
        { 0.13636363636363635, new Rational(1, 3, 22) },
        { 0.13793103448275862, new Rational(1, 4, 29) },
        { 0.14285714285714285, new Rational(1, 1, 7) },
        { 0.14814814814814814, new Rational(1, 4, 27) },
        { 0.15, new Rational(1, 3, 20) },
        { 0.15384615384615385, new Rational(1, 2, 13) },
        { 0.15789473684210525, new Rational(1, 3, 19) },
        { 0.16, new Rational(1, 4, 25) },
        { 0.16129032258064516, new Rational(1, 5, 31) },
        { 0.16666666666666666, new Rational(1, 1, 6) },
        { 0.1724137931034483, new Rational(1, 5, 29) },
        { 0.17391304347826086, new Rational(1, 4, 23) },
        { 0.17647058823529413, new Rational(1, 3, 17) },
        { 0.17857142857142858, new Rational(1, 5, 28) },
        { 0.18181818181818182, new Rational(1, 2, 11) },
        { 0.18518518518518517, new Rational(1, 5, 27) },
        { 0.1875, new Rational(1, 3, 16) },
        { 0.19047619047619047, new Rational(1, 4, 21) },
        { 0.19230769230769232, new Rational(1, 5, 26) },
        { 0.1935483870967742, new Rational(1, 6, 31) },
        { 0.2, new Rational(1, 1, 5) },
        { 0.20689655172413793, new Rational(1, 6, 29) },
        { 0.20833333333333334, new Rational(1, 5, 24) },
        { 0.21052631578947367, new Rational(1, 4, 19) },
        { 0.21428571428571427, new Rational(1, 3, 14) },
        { 0.21739130434782608, new Rational(1, 5, 23) },
        { 0.2222222222222222, new Rational(1, 2, 9) },
        { 0.22580645161290322, new Rational(1, 7, 31) },
        { 0.22727272727272727, new Rational(1, 5, 22) },
        { 0.23076923076923078, new Rational(1, 3, 13) },
        { 0.23333333333333334, new Rational(1, 7, 30) },
        { 0.23529411764705882, new Rational(1, 4, 17) },
        { 0.23809523809523808, new Rational(1, 5, 21) },
        { 0.24, new Rational(1, 6, 25) },
        { 0.2413793103448276, new Rational(1, 7, 29) },
        { 0.25, new Rational(1, 1, 4) },
        { 0.25806451612903225, new Rational(1, 8, 31) },
        { 0.25925925925925924, new Rational(1, 7, 27) },
        { 0.2608695652173913, new Rational(1, 6, 23) },
        { 0.2631578947368421, new Rational(1, 5, 19) },
        { 0.26666666666666666, new Rational(1, 4, 15) },
        { 0.2692307692307692, new Rational(1, 7, 26) },
        { 0.2727272727272727, new Rational(1, 3, 11) },
        { 0.27586206896551724, new Rational(1, 8, 29) },
        { 0.2777777777777778, new Rational(1, 5, 18) },
        { 0.28, new Rational(1, 7, 25) },
        { 0.2857142857142857, new Rational(1, 2, 7) },
        { 0.2903225806451613, new Rational(1, 9, 31) },
        { 0.2916666666666667, new Rational(1, 7, 24) },
        { 0.29411764705882354, new Rational(1, 5, 17) },
        { 0.2962962962962963, new Rational(1, 8, 27) },
        { 0.3, new Rational(1, 3, 10) },
        { 0.30434782608695654, new Rational(1, 7, 23) },
        { 0.3076923076923077, new Rational(1, 4, 13) },
        { 0.3103448275862069, new Rational(1, 9, 29) },
        { 0.3125, new Rational(1, 5, 16) },
        { 0.3157894736842105, new Rational(1, 6, 19) },
        { 0.3181818181818182, new Rational(1, 7, 22) },
        { 0.32, new Rational(1, 8, 25) },
        { 0.32142857142857145, new Rational(1, 9, 28) },
        { 0.3225806451612903, new Rational(1, 10, 31) },
        { 0.3333333333333333, new Rational(1, 1, 3) },
        { 0.3448275862068966, new Rational(1, 10, 29) },
        { 0.34615384615384615, new Rational(1, 9, 26) },
        { 0.34782608695652173, new Rational(1, 8, 23) },
        { 0.35, new Rational(1, 7, 20) },
        { 0.35294117647058826, new Rational(1, 6, 17) },
        { 0.3548387096774194, new Rational(1, 11, 31) },
        { 0.35714285714285715, new Rational(1, 5, 14) },
        { 0.36, new Rational(1, 9, 25) },
        { 0.36363636363636365, new Rational(1, 4, 11) },
        { 0.36666666666666664, new Rational(1, 11, 30) },
        { 0.3684210526315789, new Rational(1, 7, 19) },
        { 0.37037037037037035, new Rational(1, 10, 27) },
        { 0.375, new Rational(1, 3, 8) },
        { 0.3793103448275862, new Rational(1, 11, 29) },
        { 0.38095238095238093, new Rational(1, 8, 21) },
        { 0.38461538461538464, new Rational(1, 5, 13) },
        { 0.3870967741935484, new Rational(1, 12, 31) },
        { 0.3888888888888889, new Rational(1, 7, 18) },
        { 0.391304347826087, new Rational(1, 9, 23) },
        { 0.39285714285714285, new Rational(1, 11, 28) },
        { 0.4, new Rational(1, 2, 5) },
        { 0.4074074074074074, new Rational(1, 11, 27) },
        { 0.4090909090909091, new Rational(1, 9, 22) },
        { 0.4117647058823529, new Rational(1, 7, 17) },
        { 0.41379310344827586, new Rational(1, 12, 29) },
        { 0.4166666666666667, new Rational(1, 5, 12) },
        { 0.41935483870967744, new Rational(1, 13, 31) },
        { 0.42105263157894735, new Rational(1, 8, 19) },
        { 0.4230769230769231, new Rational(1, 11, 26) },
        { 0.42857142857142855, new Rational(1, 3, 7) },
        { 0.43333333333333335, new Rational(1, 13, 30) },
        { 0.43478260869565216, new Rational(1, 10, 23) },
        { 0.4375, new Rational(1, 7, 16) },
        { 0.44, new Rational(1, 11, 25) },
        { 0.4444444444444444, new Rational(1, 4, 9) },
        { 0.4482758620689655, new Rational(1, 13, 29) },
        { 0.45, new Rational(1, 9, 20) },
        { 0.45161290322580644, new Rational(1, 14, 31) },
        { 0.45454545454545453, new Rational(1, 5, 11) },
        { 0.4583333333333333, new Rational(1, 11, 24) },
        { 0.46153846153846156, new Rational(1, 6, 13) },
        { 0.4642857142857143, new Rational(1, 13, 28) },
        { 0.4666666666666667, new Rational(1, 7, 15) },
        { 0.47058823529411764, new Rational(1, 8, 17) },
        { 0.47368421052631576, new Rational(1, 9, 19) },
        { 0.47619047619047616, new Rational(1, 10, 21) },
        { 0.4782608695652174, new Rational(1, 11, 23) },
        { 0.48, new Rational(1, 12, 25) },
        { 0.48148148148148145, new Rational(1, 13, 27) },
        { 0.4827586206896552, new Rational(1, 14, 29) },
        { 0.4838709677419355, new Rational(1, 15, 31) },
        { 0.5, new Rational(1, 1, 2) },
        { 0.5161290322580645, new Rational(1, 16, 31) },
        { 0.5172413793103449, new Rational(1, 15, 29) },
        { 0.5185185185185185, new Rational(1, 14, 27) },
        { 0.52, new Rational(1, 13, 25) },
        { 0.5217391304347826, new Rational(1, 12, 23) },
        { 0.5238095238095238, new Rational(1, 11, 21) },
        { 0.5263157894736842, new Rational(1, 10, 19) },
        { 0.5294117647058824, new Rational(1, 9, 17) },
        { 0.5333333333333333, new Rational(1, 8, 15) },
        { 0.5357142857142857, new Rational(1, 15, 28) },
        { 0.5384615384615384, new Rational(1, 7, 13) },
        { 0.5416666666666666, new Rational(1, 13, 24) },
        { 0.5454545454545454, new Rational(1, 6, 11) },
        { 0.5483870967741935, new Rational(1, 17, 31) },
        { 0.55, new Rational(1, 11, 20) },
        { 0.5517241379310345, new Rational(1, 16, 29) },
        { 0.5555555555555556, new Rational(1, 5, 9) },
        { 0.56, new Rational(1, 14, 25) },
        { 0.5625, new Rational(1, 9, 16) },
        { 0.5652173913043478, new Rational(1, 13, 23) },
        { 0.5666666666666667, new Rational(1, 17, 30) },
        { 0.5714285714285714, new Rational(1, 4, 7) },
        { 0.5769230769230769, new Rational(1, 15, 26) },
        { 0.5789473684210527, new Rational(1, 11, 19) },
        { 0.5806451612903226, new Rational(1, 18, 31) },
        { 0.5833333333333334, new Rational(1, 7, 12) },
        { 0.5862068965517241, new Rational(1, 17, 29) },
        { 0.5882352941176471, new Rational(1, 10, 17) },
        { 0.5909090909090909, new Rational(1, 13, 22) },
        { 0.5925925925925926, new Rational(1, 16, 27) },
        { 0.6, new Rational(1, 3, 5) },
        { 0.6071428571428571, new Rational(1, 17, 28) },
        { 0.6086956521739131, new Rational(1, 14, 23) },
        { 0.6111111111111112, new Rational(1, 11, 18) },
        { 0.6129032258064516, new Rational(1, 19, 31) },
        { 0.6153846153846154, new Rational(1, 8, 13) },
        { 0.6190476190476191, new Rational(1, 13, 21) },
        { 0.6206896551724138, new Rational(1, 18, 29) },
        { 0.625, new Rational(1, 5, 8) },
        { 0.6296296296296297, new Rational(1, 17, 27) },
        { 0.631578947368421, new Rational(1, 12, 19) },
        { 0.6333333333333333, new Rational(1, 19, 30) },
        { 0.6363636363636364, new Rational(1, 7, 11) },
        { 0.64, new Rational(1, 16, 25) },
        { 0.6428571428571429, new Rational(1, 9, 14) },
        { 0.6451612903225806, new Rational(1, 20, 31) },
        { 0.6470588235294118, new Rational(1, 11, 17) },
        { 0.65, new Rational(1, 13, 20) },
        { 0.6521739130434783, new Rational(1, 15, 23) },
        { 0.6538461538461539, new Rational(1, 17, 26) },
        { 0.6551724137931034, new Rational(1, 19, 29) },
        { 0.6666666666666666, new Rational(1, 2, 3) },
        { 0.6774193548387096, new Rational(1, 21, 31) },
        { 0.6785714285714286, new Rational(1, 19, 28) },
        { 0.68, new Rational(1, 17, 25) },
        { 0.6818181818181818, new Rational(1, 15, 22) },
        { 0.6842105263157895, new Rational(1, 13, 19) },
        { 0.6875, new Rational(1, 11, 16) },
        { 0.6896551724137931, new Rational(1, 20, 29) },
        { 0.6923076923076923, new Rational(1, 9, 13) },
        { 0.6956521739130435, new Rational(1, 16, 23) },
        { 0.7, new Rational(1, 7, 10) },
        { 0.7037037037037037, new Rational(1, 19, 27) },
        { 0.7058823529411765, new Rational(1, 12, 17) },
        { 0.7083333333333334, new Rational(1, 17, 24) },
        { 0.7096774193548387, new Rational(1, 22, 31) },
        { 0.7142857142857143, new Rational(1, 5, 7) },
        { 0.72, new Rational(1, 18, 25) },
        { 0.7222222222222222, new Rational(1, 13, 18) },
        { 0.7241379310344828, new Rational(1, 21, 29) },
        { 0.7272727272727273, new Rational(1, 8, 11) },
        { 0.7307692307692307, new Rational(1, 19, 26) },
        { 0.7333333333333333, new Rational(1, 11, 15) },
        { 0.7368421052631579, new Rational(1, 14, 19) },
        { 0.7391304347826086, new Rational(1, 17, 23) },
        { 0.7407407407407407, new Rational(1, 20, 27) },
        { 0.7419354838709677, new Rational(1, 23, 31) },
        { 0.75, new Rational(1, 3, 4) },
        { 0.7586206896551724, new Rational(1, 22, 29) },
        { 0.76, new Rational(1, 19, 25) },
        { 0.7619047619047619, new Rational(1, 16, 21) },
        { 0.7647058823529411, new Rational(1, 13, 17) },
        { 0.7666666666666667, new Rational(1, 23, 30) },
        { 0.7692307692307693, new Rational(1, 10, 13) },
        { 0.7727272727272727, new Rational(1, 17, 22) },
        { 0.7741935483870968, new Rational(1, 24, 31) },
        { 0.7777777777777778, new Rational(1, 7, 9) },
        { 0.782608695652174, new Rational(1, 18, 23) },
        { 0.7857142857142857, new Rational(1, 11, 14) },
        { 0.7894736842105263, new Rational(1, 15, 19) },
        { 0.7916666666666666, new Rational(1, 19, 24) },
        { 0.7931034482758621, new Rational(1, 23, 29) },
        { 0.8, new Rational(1, 4, 5) },
        { 0.8064516129032258, new Rational(1, 25, 31) },
        { 0.8076923076923077, new Rational(1, 21, 26) },
        { 0.8095238095238095, new Rational(1, 17, 21) },
        { 0.8125, new Rational(1, 13, 16) },
        { 0.8148148148148148, new Rational(1, 22, 27) },
        { 0.8181818181818182, new Rational(1, 9, 11) },
        { 0.8214285714285714, new Rational(1, 23, 28) },
        { 0.8235294117647058, new Rational(1, 14, 17) },
        { 0.8260869565217391, new Rational(1, 19, 23) },
        { 0.8275862068965517, new Rational(1, 24, 29) },
        { 0.8333333333333334, new Rational(1, 5, 6) },
        { 0.8387096774193549, new Rational(1, 26, 31) },
        { 0.84, new Rational(1, 21, 25) },
        { 0.8421052631578947, new Rational(1, 16, 19) },
        { 0.8461538461538461, new Rational(1, 11, 13) },
        { 0.85, new Rational(1, 17, 20) },
        { 0.8518518518518519, new Rational(1, 23, 27) },
        { 0.8571428571428571, new Rational(1, 6, 7) },
        { 0.8620689655172413, new Rational(1, 25, 29) },
        { 0.8636363636363636, new Rational(1, 19, 22) },
        { 0.8666666666666667, new Rational(1, 13, 15) },
        { 0.8695652173913043, new Rational(1, 20, 23) },
        { 0.8709677419354839, new Rational(1, 27, 31) },
        { 0.875, new Rational(1, 7, 8) },
        { 0.88, new Rational(1, 22, 25) },
        { 0.8823529411764706, new Rational(1, 15, 17) },
        { 0.8846153846153846, new Rational(1, 23, 26) },
        { 0.8888888888888888, new Rational(1, 8, 9) },
        { 0.8928571428571429, new Rational(1, 25, 28) },
        { 0.8947368421052632, new Rational(1, 17, 19) },
        { 0.896551724137931, new Rational(1, 26, 29) },
        { 0.9, new Rational(1, 9, 10) },
        { 0.9032258064516129, new Rational(1, 28, 31) },
        { 0.9047619047619048, new Rational(1, 19, 21) },
        { 0.9090909090909091, new Rational(1, 10, 11) },
        { 0.9130434782608695, new Rational(1, 21, 23) },
        { 0.9166666666666666, new Rational(1, 11, 12) },
        { 0.92, new Rational(1, 23, 25) },
        { 0.9230769230769231, new Rational(1, 12, 13) },
        { 0.9259259259259259, new Rational(1, 25, 27) },
        { 0.9285714285714286, new Rational(1, 13, 14) },
        { 0.9310344827586207, new Rational(1, 27, 29) },
        { 0.9333333333333333, new Rational(1, 14, 15) },
        { 0.9354838709677419, new Rational(1, 29, 31) },
        { 0.9375, new Rational(1, 15, 16) },
        { 0.9411764705882353, new Rational(1, 16, 17) },
        { 0.9444444444444444, new Rational(1, 17, 18) },
        { 0.9473684210526315, new Rational(1, 18, 19) },
        { 0.95, new Rational(1, 19, 20) },
        { 0.9523809523809523, new Rational(1, 20, 21) },
        { 0.9545454545454546, new Rational(1, 21, 22) },
        { 0.9565217391304348, new Rational(1, 22, 23) },
        { 0.9583333333333334, new Rational(1, 23, 24) },
        { 0.96, new Rational(1, 24, 25) },
        { 0.9615384615384616, new Rational(1, 25, 26) },
        { 0.9629629629629629, new Rational(1, 26, 27) },
        { 0.9642857142857143, new Rational(1, 27, 28) },
        { 0.9655172413793104, new Rational(1, 28, 29) },
        { 0.9666666666666667, new Rational(1, 29, 30) },
        { 0.967741935483871, new Rational(1, 30, 31) }
    };

    static DecimalPart()
    {
        ListCount= Decimal.Count;
        Forward = Decimal.Forward.Keys.ToList();
        Backward = Decimal.Backward.Keys.ToList();
        foreach (var rational in Backward)
        {
            Console.WriteLine(rational);
        }
    }
    internal static readonly int ListCount;
    internal static List<double> Forward;
    internal static List<Rational> Backward;

    public static bool TryFindNear(double target, out int index)
    {
        var res= TryFindNearestIndex(target, out index);
        if(Math.Abs(target-Forward[index]) > Eps)
            return false;
        return res;
    }
    public static bool TryFindNearestIndex(double target, out int index)
    {
        index = -1;
        int low = 0;
        int high = ListCount - 1;
        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            if (Forward[mid] < target)
            {
                low = mid + 1;
            }
            else if (Forward[mid] > target)
            {
                high = mid - 1;
            }
            else
            {
                index = mid;
                return true;
            }
        }
        if (high < 0)
        {
            index = 0; 
            return true;
        }
        if (low >= ListCount)
        {
            index = ListCount - 1;
            return true;
        }
        double diffHigh = Math.Abs(Forward[high] - target);
        double diffLow = Math.Abs(Forward[low] - target);

        if (diffHigh <= diffLow)
        {
            index = high;
        }
        else
        {
            index = low;
        }

        return true;
    }
}
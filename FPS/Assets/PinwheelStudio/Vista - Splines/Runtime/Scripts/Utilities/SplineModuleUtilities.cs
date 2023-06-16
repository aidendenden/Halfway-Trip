#if VISTA

namespace Pinwheel.Vista.Splines
{
    public static class SplineModuleUtilities
    {
        public delegate void CollectSplinesHandler(string id, Collector<ISplineEvaluator> splines);
        public static event CollectSplinesHandler collectSplines;

        public delegate void CollectAllSplinesHandler(Collector<ISplineEvaluator> splines);
        public static event CollectAllSplinesHandler collectAllSplines;

        public static ISplineEvaluator GetFirstSplineWithId(string id)
        {
            if (collectSplines != null)
            {
                Collector<ISplineEvaluator> splines = new Collector<ISplineEvaluator>();
                collectSplines.Invoke(id, splines);
                if (splines.Count > 0)
                {
                    return splines.At(0);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static ISplineEvaluator[] GetAllSplines()
        {
            Collector<ISplineEvaluator> splines = new Collector<ISplineEvaluator>();
            collectAllSplines?.Invoke(splines);
            return splines.ToArray();
        }
    }
}
#endif

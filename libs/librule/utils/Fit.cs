namespace librule.utils
{
    static class Fit
    {
        public static string GetPattern(string name, string inputExpr, int degree, Dictionary<ushort, ushort> hash)
        {
            // 假设您有以下一组数据
            if (hash.Count == 1)
                return $"{name}=={hash.First().Key}";

            // 计算插入值
            var datas = hash.OrderBy(x => x.Key).ToArray();

            // 计算数据的N次多项式拟合
            double[] x = new double[datas.Length];
            double[] y = new double[datas.Length];
            for (var i = 0; i < datas.Length; i++)
            {
                x[i] = datas[i].Key;
                y[i] = datas[i].Value;
            }

            double[] coeffs = MathNet.Numerics.Fit.Polynomial(x, y, degree);

            // 使用多项式拟合系数预测新数据的值并得到最大和最小预测值
            var predictedValues = datas.Select(x => Polynomial.Evaluate(x.Key, coeffs)).ToArray();
            double minPredictedValue = predictedValues.Min();
            double maxPredictedValue = predictedValues.Max();

            // 得知规律
            bool isFirst = true;
            string pattern = string.Empty;
            for (int i = coeffs.Length - 1; i >= 0; i--)
            {
                var pow = string.Empty;
                for (var z = 0; z < i; z++)
                {
                    pow = pow + "*" + (isFirst ? $"({name}={inputExpr})" : name);
                    isFirst = false;
                }

                pattern += $"{(coeffs[i] >= 0 ? "+" : "-")}{(float)Math.Abs(coeffs[i])}{pow}";
            }

            // 返回判断条件
            return pattern;
        }
    }
}

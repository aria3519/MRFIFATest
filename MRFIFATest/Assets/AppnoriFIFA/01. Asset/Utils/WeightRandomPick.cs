using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using Random = System.Random;

namespace Jisu.Utils
{
    public class WeightRandomPicker<T>
    {
        private Random randomInstance = new Random();

        private readonly List<Tuple<T, int>> weightTupleList = new List<Tuple<T, int>>();

        private readonly Dictionary<T, double> itemWeightDict = new Dictionary<T, double>();
        private readonly Dictionary<T, double> normalizeWeightDict = new Dictionary<T, double>();

        private bool isDirty = true;

        private double _sumOfWeight;
        public double SumOfWeight
        {
            get
            {
                CalcSumOfWeightIfDirty();
                return _sumOfWeight;
            }
        }

        public void AddWeightItem(T item, int weight)
        {
            CheckValidWeight(weight);

            var newTuple = new Tuple<T, int>(item, weight);
            weightTupleList.Add(newTuple);
        }

        public void AddWeightItems(params (T item, int weight)[] pairs)
        {
            foreach (var pair in pairs)
            {
                AddWeightItem(pair.item, pair.weight);
            }
        }

        public void ReSeed(int seed)
        {
            randomInstance = new Random(seed);
        }

        private void CalcSumOfWeightIfDirty()
        {
            if (!isDirty)
                return;

            isDirty = false;

            var sum = 0;
            foreach (var pair in weightTupleList)
            {
                sum += pair.Item2;
            }

            _sumOfWeight = sum;

            UpdateItemWeightDict();
            UpdateNormalizedWeightDict();
        }

        private void UpdateItemWeightDict()
        {
            foreach (var pair in weightTupleList)
            {
                itemWeightDict.Add(pair.Item1, pair.Item2);
            }
        }

        private void UpdateNormalizedWeightDict()
        {
            foreach (var pair in weightTupleList)
            {
                normalizeWeightDict.Add(pair.Item1, pair.Item2 / _sumOfWeight);
            }
        }

        public double GetNormalizedWeightDict(T item)
        {
            CalcSumOfWeightIfDirty();
            return normalizeWeightDict[item];
        }

        public T GetRandomPick()
        {
            var chance = randomInstance.NextDouble();
            chance *= SumOfWeight;

            return GetRandomPick(chance);
        }

        private T GetRandomPick(double randomValue)
        {
            if (randomValue < 0f)
                randomValue = 0f;
            if (randomValue > SumOfWeight)
                randomValue = SumOfWeight - 0.00000000001f;

            double current = 0.0;
            foreach (var pair in itemWeightDict)
            {
                current += pair.Value;

                if (randomValue < current)
                {
                    return pair.Key;
                }
            }

            throw new Exception($"Unreachable - [Random Value : {randomValue}, Current Value : {current}]");
        }

        private void CheckValidWeight(in double weight)
        {
            if (weight <= 0f)
                throw new Exception("Weight value must be greater than 0.");
        }

        public void PrintGeneratedItemCount(in string printedName = "", in int maxCount = 10000)
        {
            CalcSumOfWeightIfDirty();

            var itemCntDict = new Dictionary<T, int>();
            for(int i = 0; i < itemWeightDict.Keys.Count; i++ )
                itemCntDict.Add(itemWeightDict.Keys.ToArray()[i], 0);

            for (int i = 0; i <= maxCount; i++)
                itemCntDict[GetRandomPick()]++;

            var sb = new System.Text.StringBuilder();
            sb.Append($"{printedName} Generator Total Count : {maxCount} / SumOfWeight : {SumOfWeight}\n");
            for (int i = 0; i < itemWeightDict.Keys.Count; i++)
            {
                var key = itemWeightDict.Keys.ToArray()[i];

                if(itemCntDict.TryGetValue(key, out int cnt))
                    sb.Append($"{key} : {cnt} / {(((float)cnt / maxCount) * 100f):F4}% / {GetNormalizedWeightDict(key):F5}\n");
            }

            Debug.Log(sb.ToString());
        }
    }
}

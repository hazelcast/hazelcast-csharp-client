// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Numerics;

namespace Hazelcast.Aggregators
{
    /// <summary>
    /// Creates aggregators.
    /// </summary>
    /// <remarks>
    /// Min/Max/Average aggregators are type specific, so an IntegerAvg() aggregator expects all elements to be integers.
    /// There is no conversion executed while accumulating, so if there is any other type met an exception will be thrown.
    /// <br/>
    /// In order to operate on a generic Number type use the <see cref="FixedPointSum(string)"/>,
    /// <see cref="FloatingPointSum(string)"/> and <see cref="NumberAvg(string)"/> aggregators.
    /// All of them will convert the given number to either Long or Double during the accumulation phase.
    /// It will result in a lot of allocations since each number has to be converted, but it enables the user
    /// to operate on the whole family of numbers. It is especially useful if the numbers given to the aggregators
    /// may not be of one type only.
    /// <br/>
    /// The attributePath given in the factory method allows the aggregator to operate on the value extracted by navigating
    /// to the given attributePath on each object that has been returned from a query.
    /// The attribute path may be simple, e.g. "name", or nested "address.city".
    /// <br/>
    /// If an aggregator does not accept null values pass a predicate to the aggregate call that will filter them out.
    /// <br/>
    /// If the input value or the extracted value is a collection it won't be "unfolded" - so for example
    /// count aggregation on "person.postalCodes" will return 1 for each input object and not the size of the collection.
    /// In order to calculate the size of the collection use the [any] operator, e.g. "person.postalCodes[any]".
    /// </remarks>
    public static class Aggregators
    {
        /// <summary>
        /// Counts input values (accepts nulls).
        /// </summary>
        public static IAggregator<long> Count()
        {
            return new CountAggregator();
        }

        /// <summary>
        /// Counts input values (accepts nulls).
        /// </summary>
        /// <param name="attributePath">An attribute path.</param>
        /// <remarks><para>Values are extracted from the specified <paramref name="attributePath"/>.</para></remarks>
        public static IAggregator<long> Count(string attributePath)
        {
            return new CountAggregator(attributePath);
        }

        /// <summary>
        /// Averages <see cref="double"/> input values (does not accept nulls).
        /// </summary>
        public static IAggregator<double> DoubleAvg()
        {
            return new DoubleAverageAggregator();
        }

        /// <summary>
        /// Averages <see cref="double"/> input values (does not accept nulls).
        /// </summary>
        /// <param name="attributePath">An attribute path.</param>
        /// <remarks><para>Values are extracted from the specified <paramref name="attributePath"/>.</para></remarks>
        public static IAggregator<double> DoubleAvg(string attributePath)
        {
            return new DoubleAverageAggregator(attributePath);
        }

        /// <summary>
        ///  an aggregator that calculates the average of the input values.
        /// Does NOT accept null input values.
        /// Accepts only int input values
        /// </summary>
        /// <returns></returns>
        /// <returns><see cref="IntegerAverageAggregator"/></returns>
        public static IAggregator<double> IntegerAvg()
        {
            return new IntegerAverageAggregator();
        }

        /// <summary>
        /// an aggregator that calculates the average of the input values extracted from the given attributePath.
        /// Does NOT accept null input values nor null extracted values.
        /// Accepts only int input values
        /// </summary>
        /// <param name="attributePath">attribute Path</param>
        /// <returns><see cref="IntegerAverageAggregator"/></returns>
        public static IAggregator<double> IntegerAvg(string attributePath)
        {
            return new IntegerAverageAggregator(attributePath);
        }

        /// <summary>
        /// an aggregator that calculates the average of the input values.
        /// Does NOT accept null input values.
        /// Accepts only long input values
        /// </summary>
        /// <returns><see cref="LongAverageAggregator"/></returns>
        public static IAggregator<double> LongAvg()
        {
            return new LongAverageAggregator();
        }

        /// <summary>
        /// an aggregator that calculates the average of the input values extracted from the given attributePath.
        /// Does NOT accept null input values nor null extracted values.
        /// Accepts only long input values
        /// </summary>
        /// <param name="attributePath">attribute Path</param>
        /// <returns><see cref="LongAverageAggregator"/></returns>
        public static IAggregator<double> LongAvg(string attributePath)
        {
            return new LongAverageAggregator(attributePath);
        }

        /// <summary>
        /// an aggregator that calculates the average of the input values.
        /// Does NOT accept null input values.
        /// Accepts float or double input values.
        /// </summary>
        /// <returns><see cref="NumberAverageAggregator"/></returns>
        public static IAggregator<double> NumberAvg()
        {
            return new NumberAverageAggregator();
        }

        /// <summary>
        /// an aggregator that calculates the average of the input values.
        /// Does NOT accept null input values nor null extracted values.
        /// Accepts float or double input values.
        /// </summary>
        /// <param name="attributePath">attribute Path</param>
        /// <returns><see cref="NumberAverageAggregator"/></returns>
        public static IAggregator<double> NumberAvg(string attributePath)
        {
            return new NumberAverageAggregator(attributePath);
        }

        // ---------------------------------------------------------------------------------------------------------
        // max aggregators
        // ---------------------------------------------------------------------------------------------------------

        /// <summary>
        /// an aggregator that calculates the max of the input values.
        /// Accepts null input values
        /// </summary>
        /// <returns><see cref="MaxAggregator{TResult}"/></returns>
        public static IAggregator<TResult> Max<TResult>()
        {
            return new MaxAggregator<TResult>();
        }

        /// <summary>
        /// an aggregator that calculates the max of the input values extracted from the given attributePath.
        /// Accepts null input values and null extracted values.
        /// </summary>
        /// <param name="attributePath">attribute Path</param>
        /// <returns><see cref="MaxAggregator{TResult}"/></returns>
        public static IAggregator<TResult> Max<TResult>(string attributePath)
        {
            return new MaxAggregator<TResult>(attributePath);
        }

        // ---------------------------------------------------------------------------------------------------------
        // min aggregators
        // ---------------------------------------------------------------------------------------------------------

        /// <summary>
        /// an aggregator that calculates the min of the input values.
        /// Accepts null input values
        /// </summary>
        /// <returns><see cref="MinAggregator{TResult}"/></returns>
        public static IAggregator<TResult> Min<TResult>()
        {
            return new MinAggregator<TResult>();
        }

        /// <summary>
        /// an aggregator that calculates the min of the input values extracted from the given attributePath.
        /// Accepts null input values and null extracted values.
        /// </summary>
        /// <param name="attributePath">attribute Path</param>
        /// <returns><see cref="MinAggregator{TResult}"/></returns>
        public static IAggregator<TResult> Min<TResult>(string attributePath)
        {
            return new MinAggregator<TResult>(attributePath);
        }

        // ---------------------------------------------------------------------------------------------------------
        // sum aggregators
        // ---------------------------------------------------------------------------------------------------------

        /// <summary>
        /// An aggregator that calculates the sum of the input values.
        /// Does NOT accept null input values.
        /// Accepts only BigInteger input values.
        /// </summary>
        /// <returns><see cref="BigIntegerSumAggregator"/></returns>
        public static IAggregator<BigInteger> BigIntegerSum()
        {
            return new BigIntegerSumAggregator();
        }

        /// <summary>
        /// an aggregator that calculates the sum of the input values extracted from the given attributePath.
        /// Does NOT accept null input values nor null extracted values.
        /// Accepts only BigInteger input values.
        /// </summary>
        /// <param name="attributePath">attribute Path</param>
        /// <returns><see cref="BigIntegerSumAggregator"/></returns>
        public static IAggregator<BigInteger> BigIntegerSum(string attributePath)
        {
            return new BigIntegerSumAggregator(attributePath);
        }

        /// <summary>
        /// an aggregator that calculates the sum of the input values.
        /// Does NOT accept null input values.
        /// Accepts only double input values.
        /// </summary>
        /// <returns><see cref="DoubleSumAggregator"/></returns>
        public static IAggregator<double> DoubleSum()
        {
            return new DoubleSumAggregator();
        }

        /// <summary>
        /// An aggregator that calculates the sum of the input values extracted from the given attributePath.
        /// Does NOT accept null input values nor null extracted values.
        /// Accepts only double input values.
        /// </summary>
        /// <param name="attributePath">attribute Path</param>
        /// <returns><see cref="DoubleSumAggregator"/></returns>
        public static IAggregator<double> DoubleSum(string attributePath)
        {
            return new DoubleSumAggregator(attributePath);
        }

        /// <summary>
        /// an aggregator that calculates the sum of the input values.
        /// Does NOT accept null input values.
        /// Accepts only int input values.
        /// </summary>
        /// <returns><see cref="IntegerSumAggregator"/></returns>
        public static IAggregator<long> IntegerSum()
        {
            return new IntegerSumAggregator();
        }

        /// <summary>
        /// An aggregator that calculates the sum of the input values extracted from the given attributePath.
        /// Does NOT accept null input values nor null extracted values.
        /// Accepts only int input values.
        /// </summary>
        /// <param name="attributePath">attribute Path</param>
        /// <returns><see cref="IntegerSumAggregator"/></returns>
        public static IAggregator<long> IntegerSum(string attributePath)
        {
            return new IntegerSumAggregator(attributePath);
        }

        /// <summary>
        /// an aggregator that calculates the sum of the input values.
        /// Does NOT accept null input values.
        /// Accepts only long input values.
        /// </summary>
        /// <returns><see cref="LongSumAggregator"/></returns>
        public static IAggregator<long> LongSum()
        {
            return new LongSumAggregator();
        }

        /// <summary>
        /// An aggregator that calculates the sum of the input values extracted from the given attributePath.
        /// Does NOT accept null input values nor null extracted values.
        /// Accepts only long input values.
        /// </summary>
        /// <param name="attributePath">attribute Path</param>
        /// <returns><see cref="LongSumAggregator"/></returns>
        public static IAggregator<long> LongSum(string attributePath)
        {
            return new LongSumAggregator(attributePath);
        }

        /// <summary>
        /// an aggregator that calculates the sum of the input values.
        /// Does NOT accept null input values.
        /// Accepts float or double input values.
        /// </summary>
        /// <returns><see cref="FixedSumAggregator"/></returns>
        public static IAggregator<long> FixedPointSum()
        {
            return new FixedSumAggregator();
        }

        /// <summary>
        /// An aggregator that calculates the sum of the input values extracted from the given attributePath.
        /// Does NOT accept null input values nor null extracted values.
        /// Accepts float or double input values.
        /// </summary>
        /// <param name="attributePath">attribute Path</param>
        /// <returns><see cref="FixedSumAggregator"/></returns>
        public static IAggregator<long> FixedPointSum(string attributePath)
        {
            return new FixedSumAggregator(attributePath);
        }

        /// <summary>
        /// an aggregator that calculates the sum of the input values.
        /// Does NOT accept null input values.
        /// Accepts float or double input values.
        /// </summary>
        /// <returns><see cref="FloatingPointSumAggregator"/></returns>
        public static IAggregator<double> FloatingPointSum()
        {
            return new FloatingPointSumAggregator();
        }

        /// <summary>
        /// An aggregator that calculates the sum of the input values extracted from the given attributePath.
        /// Does NOT accept null input values nor null extracted values.
        /// Accepts float or double input values.
        /// </summary>
        /// <param name="attributePath">attribute Path</param>
        /// <returns><see cref="FloatingPointSumAggregator"/></returns>
        public static IAggregator<double> FloatingPointSum(string attributePath)
        {
            return new FloatingPointSumAggregator(attributePath);
        }
    }
}
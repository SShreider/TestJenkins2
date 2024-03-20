using TestJenkins.Controllers;
using System;
using Xunit;

namespace JenkinsUnitTests
{
    public class UnitTest1
    {
        private ArithmeticOperationsController _controller; 

        public UnitTest1() 
        {
            this._controller = new ArithmeticOperationsController();
        }

        [Theory]
        [InlineData(1, 2, 3)]
        public void Addition(int x, int y, int result)
        {
            var calculation = this._controller.Sum(x, y);
            Assert.Equal(result, calculation);
        }

        [Theory]
        [InlineData(2, 7, -5)]
        public void Substraction(int x, int y, int result)
        {
            var calculation = this._controller.Sub(x, y);
            Assert.Equal(result, calculation);
        }

        [Theory]
        [InlineData(2, 7, 14)]
        public void Multiplication(int x, int y, int result)
        {
            var calculation = this._controller.Mul(x, y);
            Assert.Equal(result, calculation);
        }

        [Theory]
        [InlineData(1, 7, 0)]
        public void Division(int x, int y, int result)
        {
            var calculation = this._controller.Div(x, y);
            Assert.Equal(result, calculation);
        }
    }
}
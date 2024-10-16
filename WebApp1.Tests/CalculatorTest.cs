namespace WebApp1.Tests
{
    public class CalculatorTest
    {
        [Fact]
        public void Add_ReturnsSumOfNumbers()
        {
            var calculator = new Calculator();
            int result = calculator.Add(5, 5);

            Assert.Equal(10, result);
        }
    }
}
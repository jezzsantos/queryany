namespace CarsDomain.Entities
{
    public class CarModel
    {
        public CarModel(int year, string make, string model)
        {
            Year = year;
            Make = make;
            Model = model;
        }

        public int Year { get; }
        public string Make { get; }
        public string Model { get; }
    }
}
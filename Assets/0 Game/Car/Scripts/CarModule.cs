namespace Game.Car
{
    public abstract class CarModule : ICarModule
    {
        protected CarController _controller;

        public void SetController(CarController controller)
        {
            _controller = controller;
        }

        public abstract void OnCarInit();

        public virtual void OnCarDestroy()
        {
        }
    }
}

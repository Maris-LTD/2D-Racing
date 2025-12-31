namespace Game.Car
{
    public abstract class CarModule : ICarModule
    {
        protected ICarController _controller;

        public void SetController(ICarController controller)
        {
            _controller = controller;
        }

        public abstract void OnCarInit();

        public virtual void OnCarDestroy()
        {
        }
    }
}

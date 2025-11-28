namespace WebBrowser.Coursework.Patterns
{
    public abstract class BaseHandler
    {
        protected BaseHandler _nextHandler;

        public BaseHandler SetNext(BaseHandler handler)
        {
            _nextHandler = handler;
            return handler;
        }

        public virtual void Handle(RequestContext ctx)
        {
            if (_nextHandler != null)
            {
                _nextHandler.Handle(ctx);
            }
        }
    }
}
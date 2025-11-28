using System.Windows;

namespace WebBrowser.Coursework.Patterns
{
    public class RedirectHandler : BaseHandler
    {
        public override void Handle(RequestContext ctx)
        {
            if (ctx.StatusCode >= 300 && ctx.StatusCode < 400)
            {
                MessageBox.Show($"Виявлено перенаправлення (Code {ctx.StatusCode}).", "Redirect Handler");

                base.Handle(ctx);
            }
            else
            {
                base.Handle(ctx);
            }
        }
    }

    public class ClientErrorHandler : BaseHandler
    {
        public override void Handle(RequestContext ctx)
        {
            if (ctx.StatusCode >= 400 && ctx.StatusCode < 500)
            {
                if (ctx.StatusCode == 404)
                {
                    ctx.RenderHtml("404 Not Found", "Вибачте, але сторінку, яку ви шукаєте, не знайдено.", "orange");
                }
                else
                {
                    ctx.RenderHtml($"Error {ctx.StatusCode}", "Виникла помилка на стороні клієнта.", "orange");
                }
            }
            else
            {
                base.Handle(ctx);
            }
        }
    }

    public class ServerErrorHandler : BaseHandler
    {
        public override void Handle(RequestContext ctx)
        {
            if (ctx.StatusCode >= 500)
            {
                string msg = ctx.StatusCode == 503 ? "Сервіс тимчасово недоступний." : "Внутрішня помилка сервера.";
                ctx.RenderHtml($"Server Error {ctx.StatusCode}", msg, "red");
            }
            else
            {
                base.Handle(ctx);
            }
        }
    }

    // 4. Успішне завантаження (200 OK)
    public class SuccessHandler : BaseHandler
    {
        public override void Handle(RequestContext ctx)
        {
            if (ctx.StatusCode >= 200 && ctx.StatusCode < 400)
            {
                // ВИПРАВЛЕННЯ: Замість ctx.Browser.Source...
                // Ми викликаємо метод Load нашого компонента
                ctx.PageContent.Load(ctx.Url);
            }
            else
            {
                base.Handle(ctx);
            }
        }
    }
}
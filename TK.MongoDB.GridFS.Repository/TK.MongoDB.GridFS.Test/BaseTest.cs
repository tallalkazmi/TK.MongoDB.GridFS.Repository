using Autofac;
using TK.MongoDB.GridFS.Data;
using TK.MongoDB.GridFS.Test.Models;

namespace TK.MongoDB.GridFS.Test
{
    public class BaseTest
    {
        private IContainer autofacContainer;
        protected IContainer AutofacContainer
        {
            get
            {
                if (autofacContainer == null)
                {
                    var builder = new ContainerBuilder();

                    builder.RegisterGeneric(typeof(FileRepository<>))
                        .As(typeof(IFileRepository<>))
                        .InstancePerLifetimeScope();

                    var container = builder.Build();
                    autofacContainer = container;
                }

                return autofacContainer;
            }
        }

        protected IFileRepository<Image> ImageRepository
        {
            get
            {
                return AutofacContainer.Resolve<IFileRepository<Image>>();
            }
        }

        protected IFileRepository<Document> DocumentRepository
        {
            get
            {
                return AutofacContainer.Resolve<IFileRepository<Document>>();
            }
        }
    }
}

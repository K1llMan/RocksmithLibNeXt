using Microsoft.Extensions.Logging;

using RocksmithLibNeXt.Common;

namespace RocksmithLibNeXt.Formats.Common
{
    public class Loggable
    {
        #region Properties

        protected ILogger Logger { get; }

        #endregion Properties

        #region Main functions

        public Loggable()
        {
            Logger = LoggerCreator.Create(GetType());
        }

        #endregion Main functions
    }
}
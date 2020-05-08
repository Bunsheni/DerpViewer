using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerpViewer.Services
{
    public class DerpImageFileClassificationService
    {
        private DerpFileService fileService;

        public DerpImageFileClassificationService(DerpFileService fileService)
        {
            this.fileService = fileService;
        }

    }
}

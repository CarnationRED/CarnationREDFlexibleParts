using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarnationVariableSectionPart.UI
{

    public class CVSPResourceSwitcher : Switcher<string>
    {
        public static event OnGetRealFuelInstalledHandler OnGetRealFuelInstalled;
        public delegate bool OnGetRealFuelInstalledHandler();
        
        protected void Start()
        {
            Init(buttonUsedForScroll: true);
            if (OnGetRealFuelInstalled.Invoke())
            {
                gameObject.SetActive(false);
                Destroy(scrollRect.gameObject);
            }
        }
        protected override string ToString(string s) => s;
    }
}

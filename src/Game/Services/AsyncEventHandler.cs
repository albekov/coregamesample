using System.Threading.Tasks;

namespace Game.Services
{
    public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e);
}
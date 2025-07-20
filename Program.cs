using System;
using Observer;

class Program
{
    static void Main()
    {
        RealObject realObject = new RealObject();
        realObject.Attach(new RealObserver("sanya.kruty1@gmail.com"));
        realObject.SendMessage("Саня крутий IТ-шнiк, вiн молодець, так тримати!!!");

    }
}
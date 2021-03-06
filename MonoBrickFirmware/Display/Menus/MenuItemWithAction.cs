using System;
using MonoBrickFirmware.Display;

namespace MonoBrickFirmware.Display.Menus
{
	public enum MenuItemSymbole {None, LeftArrow, RightArrow};
	
	public class MenuItemWithAction : IMenuItem
	{
		private string text;
		private Lcd lcd;
		private Func<bool> action;
		private MenuItemSymbole symbole;
		private const int arrowEdge = 4;
		private const int arrowOffset = 4;
		public MenuItemWithAction(Lcd lcd, string text, Func<bool> action, MenuItemSymbole symbole = MenuItemSymbole.None){
			this.text = text;
			this.action = action;
			this.lcd = lcd;
			this.symbole = symbole;
		}
		public bool EnterAction()
		{
			return action();
		}
		public bool LeftAction (){return false;}
		public bool RightAction(){return false;}
		public void Draw (Font f, Rectangle r, bool color)
		{
			lcd.WriteTextBox (f, r, text, color);
			if (symbole == MenuItemSymbole.LeftArrow || symbole == MenuItemSymbole.RightArrow) {
				int arrowWidth =(int) f.maxWidth/3;
				Rectangle arrowRect = new Rectangle(new Point(r.P2.X -(arrowWidth+arrowOffset), r.P1.Y + arrowEdge), new Point(r.P2.X -arrowOffset, r.P2.Y-arrowEdge));
				if(symbole == MenuItemSymbole.LeftArrow)
					lcd.DrawArrow(arrowRect,Lcd.ArrowOrientation.Left, color);
				else
					lcd.DrawArrow(arrowRect,Lcd.ArrowOrientation.Right, color);
			}
		}	
	}
}


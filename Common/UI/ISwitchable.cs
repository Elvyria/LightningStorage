namespace LightningStorage.Common.UI;

interface ISwitchable
{
	void Open(bool animate = false);
	void Close(bool animate = false);
}

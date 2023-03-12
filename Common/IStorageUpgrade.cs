namespace MagicStorage.Common;

public interface IStorageUpgrade
{
	public void Upgrade(int i, int j);
	public bool CanUpgrade(int i, int j);
}

public interface IUpgradeable
{

}

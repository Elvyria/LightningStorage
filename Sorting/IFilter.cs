namespace MagicStorage.Sorting
{
	public interface IFilter<in T> {
		bool Passes(T x);
	}
}


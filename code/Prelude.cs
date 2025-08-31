namespace Prelude;

public abstract record Maybe<T>;

public record Nothing<T>() : Maybe<T>;

public record Just<T>(T a) : Maybe<T>;

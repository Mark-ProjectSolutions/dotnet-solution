using backend.domain.Errors;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.domain.ValueObjects
{
    public class Result
    {
        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException("Success result cannot carry an error.");
            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException("Failure result must carry an error.");

            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);

        public static Result<T> Success<T>(T value) => new(value, true, Error.None);
        public static Result<T> Failure<T>(Error e) => new(default!, false, e);
    }

    public sealed class Result<T> : Result
    {
        private readonly T _value;

        internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error)
            => _value = value;

        public T Value => IsSuccess
            ? _value
            : throw new InvalidOperationException("Cannot access Value of a failed Result.");

        public static implicit operator Result<T>(T value) => Success<T>(value);
    }
}

using System;

namespace Railyard.Operations;

/// <summary>
/// Represents an exception thrown when an input is invalid or unrecognized.
/// This exception includes the type of the input that caused the failure, allowing for
/// more precise error handling based on the type of the input.
/// </summary>
/// <param name="inputType">The type of the input that was not recognized or is invalid.</param>
public class InvalidInputException(Type inputType) : Exception($"The input was not recognized. As a {inputType.Name}.");

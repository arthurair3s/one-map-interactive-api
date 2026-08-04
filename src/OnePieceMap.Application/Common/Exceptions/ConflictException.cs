namespace OnePieceMap.Application.Common.Exceptions;

// RN01 (exclusão protegida), RN02/RN03/RN04 (validação amigável de unicidade), etc.
public class ConflictException(string message) : Exception(message);

using System;

namespace RentingApi.Application.Dtos;

public record VehicleDto(
    Guid Id,
    string LicensePlate,
    string Brand,
    string Model,
    int Year,
    DateTime ManufactureDate,
    string Status
);

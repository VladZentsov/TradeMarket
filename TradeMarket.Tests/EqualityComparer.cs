using Data.Entities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Business.Models;

namespace TradeMarket.Tests
{
    internal class CustomerEqualityComparer : IEqualityComparer<Customer>
    {
        public bool Equals([AllowNull] Customer x, [AllowNull] Customer y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Id == y.Id
                && x.PersonId == y.PersonId
                && x.DiscountValue == y.DiscountValue;
        }

        public int GetHashCode([DisallowNull] Customer obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class ReceiptEqualityComparer : IEqualityComparer<Receipt>
    {
        public bool Equals([AllowNull] Receipt x, [AllowNull] Receipt y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Id == y.Id
                && x.CustomerId == y.CustomerId
                && x.OperationDate == y.OperationDate
                && x.IsCheckedOut == y.IsCheckedOut;
        }

        public int GetHashCode([DisallowNull] Receipt obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class ProductEqualityComparer : IEqualityComparer<Product>
    {
        public bool Equals([AllowNull] Product x, [AllowNull] Product y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Id == y.Id
                && x.ProductCategoryId == y.ProductCategoryId
                && x.ProductName == y.ProductName
                && x.Price == y.Price;
        }

        public int GetHashCode([DisallowNull] Product obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class PersonEqualityComparer : IEqualityComparer<Person>
    {
        public bool Equals([AllowNull] Person x, [AllowNull] Person y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Id == y.Id
                && x.Name == y.Name
                && x.Surname == y.Surname
                && x.BirthDate == y.BirthDate;
        }

        public int GetHashCode([DisallowNull] Person obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class ProductCategoryEqualityComparer : IEqualityComparer<ProductCategory>
    {
        public bool Equals([AllowNull] ProductCategory x, [AllowNull] ProductCategory y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Id == y.Id
                && x.CategoryName == y.CategoryName;
        }

        public int GetHashCode([DisallowNull] ProductCategory obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class ReceiptDetailEqualityComparer : IEqualityComparer<ReceiptDetail>
    {
        public bool Equals([AllowNull] ReceiptDetail x, [AllowNull] ReceiptDetail y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Id == y.Id
                && x.ReceiptId == y.ReceiptId
                && x.ProductId == y.ProductId
                && x.UnitPrice == y.UnitPrice
                && x.DiscountUnitPrice == y.DiscountUnitPrice
                && x.Quantity == y.Quantity;
        }

        public int GetHashCode([DisallowNull] ReceiptDetail obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class CustomerModelEqualityComparer : IEqualityComparer<CustomerModel>
    {
        public bool Equals([AllowNull] CustomerModel x, [AllowNull] CustomerModel y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Id == y.Id &&
                x.BirthDate == y.BirthDate &&
                x.DiscountValue == y.DiscountValue &&
                string.Equals(x.Name, y.Name) &&
                string.Equals(x.Surname, y.Surname);
        }

        public int GetHashCode([DisallowNull] CustomerModel obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class ProductModelEqualityComparer : IEqualityComparer<ProductModel>
    {
        public bool Equals([AllowNull] ProductModel x, [AllowNull] ProductModel y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            return x.Id == y.Id &&
                x.ProductCategoryId == y.ProductCategoryId &&
                string.Equals(x.CategoryName, y.CategoryName) &&
                string.Equals(x.ProductName, y.ProductName) &&
                x.Price == y.Price;
        }

        public int GetHashCode([DisallowNull] ProductModel obj)
        {
            return obj.GetHashCode();
        }
    }
}

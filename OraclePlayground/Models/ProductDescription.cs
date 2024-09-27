﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OraclePlayground.Models;

/// <summary>
/// Product descriptions in several languages.
/// </summary>
[Table("ProductDescription", Schema = "SalesLT")]
[Index("Rowguid", Name = "AK_ProductDescription_rowguid", IsUnique = true)]
public partial class ProductDescription
{
    /// <summary>
    /// Primary key for ProductDescription records.
    /// </summary>
    [Key]
    [Column("ProductDescriptionID")]
    public int ProductDescriptionId { get; set; }

    /// <summary>
    /// Description of the product.
    /// </summary>
    [Required]
    [StringLength(400)]
    public string Description { get; set; }

    /// <summary>
    /// ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.
    /// </summary>
    [Column("rowguid")]
    public Guid Rowguid { get; set; }

    /// <summary>
    /// Date and time the record was last updated.
    /// </summary>
    [Column(TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [InverseProperty("ProductDescription")]
    public virtual ICollection<ProductModelProductDescription> ProductModelProductDescriptions { get; set; } = new List<ProductModelProductDescription>();
}
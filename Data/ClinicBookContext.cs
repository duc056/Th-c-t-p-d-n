using System;
using System.Collections.Generic;
using ClinicBook.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicBook.Data;

public partial class ClinicBookContext : DbContext
{
    public ClinicBookContext(DbContextOptions<ClinicBookContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BacSi> BacSis { get; set; }

    public virtual DbSet<BacSiChuyenKhoa> BacSiChuyenKhoas { get; set; }

    public virtual DbSet<BenhNhan> BenhNhans { get; set; }

    public virtual DbSet<CaLamViec> CaLamViecs { get; set; }

    public virtual DbSet<ChuyenKhoa> ChuyenKhoas { get; set; }

    public virtual DbSet<KetQuaKham> KetQuaKhams { get; set; }

    public virtual DbSet<LichHen> LichHens { get; set; }

    public virtual DbSet<LichSuLichHen> LichSuLichHens { get; set; }

    public virtual DbSet<NgayNghi> NgayNghis { get; set; }

    public virtual DbSet<PhongKham> PhongKhams { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BacSi>(entity =>
        {
            entity.HasKey(e => e.MaBacSi).HasName("PK__BacSi__E022715EECDDDC3B");

            entity.Property(e => e.KinhNghiem).HasDefaultValue(0);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaPhongKhamNavigation).WithMany(p => p.BacSis)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BacSi_PhongKham");

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithOne(p => p.BacSi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BacSi_TaiKhoan");
        });

        modelBuilder.Entity<BacSiChuyenKhoa>(entity =>
        {
            entity.HasOne(d => d.MaBacSiNavigation).WithMany(p => p.BacSiChuyenKhoas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BSCK_BacSi");

            entity.HasOne(d => d.MaChuyenKhoaNavigation).WithMany(p => p.BacSiChuyenKhoas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BSCK_ChuyenKhoa");
        });

        modelBuilder.Entity<BenhNhan>(entity =>
        {
            entity.HasKey(e => e.MaBenhNhan).HasName("PK__BenhNhan__22A8B330E3F0FC68");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithOne(p => p.BenhNhan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BenhNhan_TaiKhoan");
        });

        modelBuilder.Entity<CaLamViec>(entity =>
        {
            entity.HasKey(e => e.MaCaLamViec).HasName("PK__CaLamVie__E545F62578BE38A7");

            entity.Property(e => e.ThoiLuongKham).HasDefaultValue(30);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaBacSiNavigation).WithMany(p => p.CaLamViecs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CaLamViec_BacSi");
        });

        modelBuilder.Entity<ChuyenKhoa>(entity =>
        {
            entity.HasKey(e => e.MaChuyenKhoa).HasName("PK__ChuyenKh__CD0E428FEB1BD71E");

            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<KetQuaKham>(entity =>
        {
            entity.HasKey(e => e.MaKetQua).HasName("PK__KetQuaKh__D5B3102A3D17E6D3");

            entity.Property(e => e.NgayKham).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.MaBacSiNavigation).WithMany(p => p.KetQuaKhams)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KetQuaKham_BacSi");

            entity.HasOne(d => d.MaLichHenNavigation).WithOne(p => p.KetQuaKham)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KetQuaKham_LichHen");
        });

        modelBuilder.Entity<LichHen>(entity =>
        {
            entity.HasKey(e => e.MaLichHen).HasName("PK__LichHen__150F264FAF74D8AD");

            entity.Property(e => e.NgayDat).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai).HasDefaultValue("ChoXacNhan");

            entity.HasOne(d => d.MaBacSiNavigation).WithMany(p => p.LichHens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichHen_BacSi");

            entity.HasOne(d => d.MaBenhNhanNavigation).WithMany(p => p.LichHens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichHen_BenhNhan");

            entity.HasOne(d => d.MaPhongKhamNavigation).WithMany(p => p.LichHens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichHen_PhongKham");
        });

        modelBuilder.Entity<LichSuLichHen>(entity =>
        {
            entity.HasKey(e => e.MaLichSu).HasName("PK__LichSuLi__C443222A8B0517B5");

            entity.Property(e => e.ThoiGianThayDoi).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.MaLichHenNavigation).WithMany(p => p.LichSuLichHens)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LichSu_LichHen");

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.LichSuLichHens).HasConstraintName("FK_LichSu_TaiKhoan");
        });

        modelBuilder.Entity<NgayNghi>(entity =>
        {
            entity.HasKey(e => e.MaNgayNghi).HasName("PK__NgayNghi__526A7F78AE1CA598");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.MaBacSiNavigation).WithMany(p => p.NgayNghis)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NgayNghi_BacSi");
        });

        modelBuilder.Entity<PhongKham>(entity =>
        {
            entity.HasKey(e => e.MaPhongKham).HasName("PK__PhongKha__D31EFFBA841FF085");

            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C652942BCA825");

            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.TaiKhoans)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaiKhoan_VaiTro");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CFF668F1F5");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QuickRoomSolutions.Connections;
using QuickRoomSolutions.Models;

namespace QuickRoomSolutions.DataBase;

public partial class QuickRoomSolutionDatabaseContext : DbContext
{
    public QuickRoomSolutionDatabaseContext()
    {
    }

    public QuickRoomSolutionDatabaseContext(DbContextOptions<QuickRoomSolutionDatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cargo> Cargos { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Fornecedor> Fornecedores { get; set; }

    public virtual DbSet<Funcionario> Funcionarios { get; set; }

    public virtual DbSet<FuncionarioFornecedor> FuncionarioFornecedores { get; set; }

    public virtual DbSet<Orcamento> Orcamentos { get; set; }

    public virtual DbSet<Pessoa> Pessoas { get; set; }

    public virtual DbSet<Quarto> Quartos { get; set; }

    public virtual DbSet<TicketLimpeza> TicketsLimpeza { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Servico> Servicos { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Tipologia> Tipologia { get; set; }


    /********************************NÃO MEXER PRETTY PLEASE********************************/
    //
    //SqlServerDbConnection conection = new SqlServerDbConnection();
    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseSqlServer(conection.GetConnectionString());
    /***************************************************************************************/
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.HasKey(e => e.CargoId).HasName("PK__Cargo__B4E665CDEED8E657");

            entity.ToTable("Cargo");

            entity.Property(e => e.DescricaoCargo)
                .HasMaxLength(255)
                .IsUnicode(false);
        });


        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.ClienteId).HasName("PK__Cliente__71ABD087D82EA67A");

            entity.ToTable("Cliente");

            entity.Property(e => e.ClientPassword)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PessoaNif).HasColumnName("PessoaNIF");

            entity.HasOne(d => d.PessoaPessoa).WithOne(p => p.ClienteCliente)
                .HasForeignKey<Cliente>(d => d.PessoaNif)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKCliente785426");

        });

        modelBuilder.Entity<Fornecedor>(entity =>
        {
            entity.HasKey(e => e.FornecedorId).HasName("PK__Forneced__494B8C10371A8538");

            entity.ToTable("Fornecedor");

            entity.Property(e => e.FornecedorEmail)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FornecedorMorada)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.FornecedorNipc).HasColumnName("FornecedorNIPC");
            entity.Property(e => e.FornecedorNome)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasMany(d => d.ServicosServicos).WithMany(p => p.FornecedorFornecedors)
                .UsingEntity<Dictionary<string, object>>(
                    "ServicosFornecedor",
                    r => r.HasOne<Servico>().WithMany()
                        .HasForeignKey("ServicosServicoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FKServicosFo3255"),
                    l => l.HasOne<Fornecedor>().WithMany()
                        .HasForeignKey("FornecedorFornecedorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FKServicosFo466425"),
                    j =>
                    {
                        j.HasKey("FornecedorFornecedorId", "ServicosServicoId").HasName("PK__Servicos__87388612C1D67A14");
                        j.ToTable("ServicosFornecedor");
                    });
        });

        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.HasKey(e => e.FuncionarioId).HasName("PK__Funciona__297ECCAA3C870B15");

            entity.ToTable("Funcionario");

            entity.Property(e => e.FuncionarioPassword)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Nif).HasColumnName("NIF");

            entity.HasOne(d => d.CargoCargo).WithMany(p => p.Funcionarios)
                .HasForeignKey(d => d.CargoCargoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKFuncionari942921");

            entity.HasOne(d => d.PessoaPessoa).WithOne(p => p.FuncionarioFuncionario)
                .HasForeignKey<Funcionario>(d => d.Nif)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKFuncionari583978");
        });

        modelBuilder.Entity<FuncionarioFornecedor>(entity =>
        {
            entity.HasKey(e => e.FuncFornecedorId).HasName("PK__Funciona__DE2495F54369F4D4");

            entity.ToTable("FuncionarioFornecedor");

            entity.Property(e => e.FuncFornecedorPassword)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.PessoaNif).HasColumnName("PessoaNIF");

            entity.HasOne(d => d.FornecedorFornecedor).WithMany(p => p.FuncionarioFornecedors)
                .HasForeignKey(d => d.FornecedorFornecedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKFuncionari357614");

            entity.HasOne(d => d.PessoaPessoa).WithOne(p => p.FuncionarioFornecedorFuncionarioFornecedor)
                .HasForeignKey<FuncionarioFornecedor>(d => d.PessoaNif)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKFuncionari252582");
        });


        modelBuilder.Entity<Orcamento>(entity =>
        {
            entity.HasKey(e => e.OrcamentoId).HasName("PK__Orcament__4E96F7795B2C316B");

            entity.ToTable("Orcamento");

            entity.Property(e => e.OrcamentoEstado).HasConversion<string>();
            entity.Property(e => e.OrcamentoEstado)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.DescricacaoOrcamento)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.HasOne(d => d.FornecedorFornecedor).WithMany(p => p.Orcamentos)
                .HasForeignKey(d => d.FornecedorFornecedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKOrcamento670984");

            entity.HasOne(d => d.TicketTicket).WithMany(p => p.Orcamentos)
                .HasForeignKey(d => d.TicketTicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKOrcamento101725");
        });

        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.HasKey(e => e.Nif).HasName("PK__Pessoa__C7DEC3317E9C9F41");

            entity.ToTable("Pessoa");

            entity.HasIndex(e => e.Nif, "UQ__Pessoa__C7DEC33027126182").IsUnique();

            entity.Property(e => e.Nif)
                .ValueGeneratedNever()
                .HasColumnName("NIF");
            entity.Property(e => e.Cp)
                .HasMaxLength(8)
                .IsUnicode(false)
                .HasColumnName("CP");
            entity.Property(e => e.DataNasc).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Morada)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Nome)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Quarto>(entity =>
        {
            entity.HasKey(e => e.QuartoId).HasName("PK__Quarto__903445333972E660");

            entity.Property(quarto => quarto.QuartoEstado).HasConversion<string>();

            entity.Property(e => e.QuartoEstado)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.ToTable("Quarto");

            entity.Property(e => e.QuartoId)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Bloco)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.TipologiaTipologia).WithMany(p => p.Quartos)
                .HasForeignKey(d => d.TipologiaTipologiaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKQuarto389558");
        });


        modelBuilder.Entity<TicketLimpeza>(entity =>
        {
            entity.HasKey(e => e.LimpezaId).HasName("PK__RegistoL__15039A162BB27612");

            entity.ToTable("TicketLimpeza");

            entity.Property(e => e.LimpezaEstado).HasConversion<string>();
            entity.Property(e => e.LimpezaEstado)
                .HasMaxLength(255)
                .IsUnicode(false);


            entity.Property(e => e.LimpezaDataCriacao).HasColumnType("datetime");

            entity.Property(e => e.LimpezaDataAtualizacao).HasColumnType("datetime");


            entity.Property(e => e.QuartoQuartoId)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.FuncionarioFuncionario).WithMany(p => p.TicketsLimpezaRececionista)
                .HasForeignKey(d => d.FuncionarioFuncionarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKRegistoLim395939");

            entity.HasOne(d => d.FuncionarioLimpeza).WithMany(p => p.TicketsLimpezaAuxLimpeza)
                .HasForeignKey(d => d.FuncionarioLimpezaId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.QuartoQuarto).WithMany(p => p.TicketsLimpeza)
                .HasForeignKey(d => d.QuartoQuartoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKRegistoLim780577");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            
            entity.HasKey(e => e.ReservaId).HasName("PK__Reserva__C39937634EF16361");

            entity.ToTable("Reserva");

            entity.Property(reserva => reserva.EstadoReserva).HasConversion<string>();

            entity.Property(e => e.DataCheckIn).HasColumnType("datetime");
            entity.Property(e => e.DataCheckOut).HasColumnType("datetime");
            entity.Property(e => e.DataFim).HasColumnType("datetime");
            entity.Property(e => e.DataInicio).HasColumnType("datetime");
            entity.Property(e => e.EstadoReserva)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.QuartoQuartoId)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.ClienteCliente).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.ClienteClienteId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FKReserva817646");

            entity.HasOne(d => d.QuartoQuarto).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.QuartoQuartoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FKReserva851797");

            entity.HasOne(d => d.TipologiaTipologia).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.TipologiaId)
                .OnDelete(DeleteBehavior.SetNull);



        });

        modelBuilder.Entity<Servico>(entity =>
        {
            entity.HasKey(e => e.ServicoId).HasName("PK__Servicos__C59767B6FC5CF0A1");

            entity.Property(e => e.ServicoTipo)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Ticket__712CC6076586CA3E");

            entity.ToTable("Ticket");

            entity.Property(ticket => ticket.TicketEstado).HasConversion<string>();
            entity.Property(e => e.TicketEstado)
                .HasMaxLength(255)
                .IsUnicode(false);



            entity.Property(e => e.QuartoQuartoId)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.TickectDataAtualizacao).HasColumnType("datetime");

            entity.Property(e => e.TicketDataAbertura).HasColumnType("datetime");

            entity.Property(e => e.TicketDescricao)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.FuncionarioFuncionario).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.FuncionarioFuncionarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKTicket380775");

            entity.HasOne(d => d.QuartoQuarto).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.QuartoQuartoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKTicket996136");


            entity.HasOne(d => d.FuncionarioFornecedor).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.FuncionarioFornecedorFuncFornecedorId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ServicoServico).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ServicoId)
                .OnDelete(DeleteBehavior.ClientSetNull);

        });


        modelBuilder.Entity<Tipologia>(entity =>
        {
            entity.HasKey(e => e.TipologiaId).HasName("PK__Tipologi__62F8D35E25BE44C9");

            entity.Property(e => e.TipologiaDescricao)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

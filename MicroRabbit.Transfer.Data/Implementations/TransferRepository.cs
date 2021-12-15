using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Domain.Interfaces;
using MicroRabbit.Transfer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Transfer.Data.Implementations
{
    public class TransferRepository : ITransferRepository
    {
        private readonly TransferDbContext _db;

        public TransferRepository(TransferDbContext db)
        {
            _db = db;
        }

        public void Add(TransferLog transferLog)
        {
            _db.TransferLogs.Add(transferLog);
            _db.SaveChanges();
        }

        public IEnumerable<TransferLog> GetAllTransferLog()
        {
            return _db.TransferLogs;
        }
    }
}

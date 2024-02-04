using api.Data;
using AutoMapper;
using fragrancehaven_api.Interfaces;

namespace fragrancehaven_api.Data
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public ReviewRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
    }
}
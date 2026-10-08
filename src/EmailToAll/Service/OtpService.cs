using EmailToAll.Data;
using EmailToAll.Data.Interface;
using EmailToAll.Data.Repository;
using EmailToAll.Models;

namespace EmailToAll.Service
{
    public class OtpService 
    {
        private readonly IUnitOfWork _unitOfWork;

        public OtpService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreateAsync(OtpVerification otpVerification)
        {
            try
            {
                var res = string.Empty;
                await _unitOfWork.Otp.CreateAsync(otpVerification);
                await _unitOfWork.SaveAsync();

                return "Add Successfuly";

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<OtpVerification>> GetAllAsync()
        {
            try
            {
                var res =await _unitOfWork.Otp.GetAllAsync();
                return res;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<OtpVerification> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(OtpVerification otpVerification)
        {
            throw new NotImplementedException();
        }
    }
}

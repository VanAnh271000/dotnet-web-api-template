namespace Infrastructure.BackgroundJobs.Jobs
{
    public class CleanUpJob
    {
        public CleanUpJob()
        {
        }

        public async Task ExecuteAsync()
        {
            // cleanup logic
            // call to service layer to perform cleanup
        }
    }
}

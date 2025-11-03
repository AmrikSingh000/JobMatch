using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobMatch.Migrations
{
    public partial class UseEmploymentTypeEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.JobApplications','CvFilePath') IS NOT NULL
    ALTER TABLE dbo.JobApplications DROP COLUMN CvFilePath;
IF COL_LENGTH('dbo.JobApplications','NotesForRecruiter') IS NOT NULL
    ALTER TABLE dbo.JobApplications DROP COLUMN NotesForRecruiter;
");

            
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Jobs','EmploymentType') IS NULL
BEGIN
    ALTER TABLE dbo.Jobs 
        ADD EmploymentType int NOT NULL CONSTRAINT DF_Jobs_EmploymentType DEFAULT(1);
END
");

            
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Jobs','JobType') IS NOT NULL
BEGIN
    UPDATE J SET EmploymentType = 1 FROM dbo.Jobs J WHERE J.JobType IN ('Full-time','FullTime');
    UPDATE J SET EmploymentType = 2 FROM dbo.Jobs J WHERE J.JobType IN ('Part-time','PartTime');
    UPDATE J SET EmploymentType = 3 FROM dbo.Jobs J WHERE J.JobType = 'Contract';
    UPDATE J SET EmploymentType = 4 FROM dbo.Jobs J WHERE J.JobType = 'Internship';
    UPDATE J SET EmploymentType = 5 FROM dbo.Jobs J WHERE J.JobType = 'Remote';
END
");

            
            migrationBuilder.AlterColumn<string>(
                name: "ApplicantUserId",
                table: "JobApplications",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.JobApplications','CoverLetter') IS NULL
BEGIN
    ALTER TABLE dbo.JobApplications
        ADD CoverLetter nvarchar(4000) NULL;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.JobApplications','CoverLetter') IS NOT NULL
    ALTER TABLE dbo.JobApplications DROP COLUMN CoverLetter;
");

            
            migrationBuilder.AlterColumn<string>(
                name: "ApplicantUserId",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.JobApplications','CvFilePath') IS NULL
    ALTER TABLE dbo.JobApplications ADD CvFilePath nvarchar(512) NULL;
IF COL_LENGTH('dbo.JobApplications','NotesForRecruiter') IS NULL
    ALTER TABLE dbo.JobApplications ADD NotesForRecruiter nvarchar(2048) NULL;
");

            
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Jobs','EmploymentType') IS NOT NULL
BEGIN
    DECLARE @df NVARCHAR(128);
    SELECT @df = d.name
    FROM sys.default_constraints d
    JOIN sys.columns c
      ON d.parent_object_id = c.object_id
     AND d.parent_column_id = c.column_id
    WHERE d.parent_object_id = OBJECT_ID('dbo.Jobs')
      AND c.name = 'EmploymentType';

    IF @df IS NOT NULL
        EXEC('ALTER TABLE dbo.Jobs DROP CONSTRAINT ' + QUOTENAME(@df));

    ALTER TABLE dbo.Jobs DROP COLUMN EmploymentType;
END
");
        }
    }
}

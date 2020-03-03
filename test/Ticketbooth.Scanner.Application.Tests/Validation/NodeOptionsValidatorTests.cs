using FluentValidation.TestHelper;
using NUnit.Framework;
using Ticketbooth.Scanner.Application.Validation;

namespace Ticketbooth.Scanner.Application.Tests.Validation
{
    public class NodeOptionsValidatorTests
    {
        private NodeOptionsValidator _nodeOptionsValidator;

        [SetUp]
        public void SetUp()
        {
            _nodeOptionsValidator = new NodeOptionsValidator();
        }

        [Test]
        public void ApiUri_ValidFormat()
        {
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ApiUri, "https://192.168.1.100");
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ApiUri, "https://my.fullnode.cirrus");
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ApiUri, "https://my.fullnode.cirrus:80");
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ApiUri, "https://my.fullnode.cirrus/:80");
        }

        [Test]
        public void ApiUri_InvalidFormat()
        {
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ApiUri, "192.168.1.100");
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ApiUri, "http://my.fullnode.cirrus:80");
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ApiUri, "ftp://192.168.1.100");
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ApiUri, "file://192.168.1.100");
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ApiUri, "https://");
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ApiUri, "ahttps://");
        }

        [Test]
        public void ContractAddress_ValidFormat()
        {
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ContractAddress, "123456789abcdefhijkmnopqrs");
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ContractAddress, "tuvwxyzABCDEFGHJKLMNPQRSTUV");
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ContractAddress, "tuvwxyzABCDEFGHJKLMNPQRSTUVW");
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ContractAddress, "tuvwxyzABCDEFGHJKLMNPQRSTUVW");
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ContractAddress, "tuvwxyzABCDEFGHJKLMNPQRSTUVWXY");
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ContractAddress, "tuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ");
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ContractAddress, "tuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ1");
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ContractAddress, "tuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ12");
            _nodeOptionsValidator.ShouldNotHaveValidationErrorFor(validator => validator.ContractAddress, "tuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ13");
        }

        [Test]
        public void ContractAddress_InvalidFormat()
        {
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ContractAddress, null as string);
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ContractAddress, string.Empty);
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ContractAddress, "tFLLYMPRCCy239bAtT3fr9GUM");
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ContractAddress, "                                   ");
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ContractAddress, "tFLLYMPRCCy2 9bAtT3fr9GUMwiTs2DKMN");
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ContractAddress, "tFLLYMPRCCy239bAtT3f09GUMwiTs2DKMN");
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ContractAddress, "tFLLYMPRCCy239bAtT3fO9GUMwiTs2DKMN");
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ContractAddress, "tFLLYMPRCCy239bAtT3fr9GUMwiTs2DKIN");
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ContractAddress, "tFLLYMPRCCy239bAtT3fr9GUMwiTs2DKlN");
            _nodeOptionsValidator.ShouldHaveValidationErrorFor(validator => validator.ContractAddress, "tFLLYMPRCCy239bAtT3fr9GUMwiTs2DKMNe");
        }
    }
}

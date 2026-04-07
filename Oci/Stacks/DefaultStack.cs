using Pulumi;
using Pulumi.Oci.Core;

namespace Oci.Stacks;

public class DefaultStack : Stack
{
    public DefaultStack()
    {
        var config = new Config();
        var compartmentOcid = config.Require("compartmentOcid");
        var vcnCidrBlock = config.Require("vcnCidrBlock");
        var subnetCidrBlock = config.Require("subnetCidrBlock");
        var ubuntu24ImageOcid = config.Require("ubuntu24ImageOcid");

        var vcn = new Vcn(
            "temasek3-prod-vcn",
            new VcnArgs
            {
                CompartmentId = compartmentOcid,
                CidrBlock = vcnCidrBlock,
                DisplayName = "temasek3-prod-vcn",
            }
        );

        var subnet = new Subnet(
            "temasek3-prod-subnet",
            new SubnetArgs
            {
                CompartmentId = compartmentOcid,
                VcnId = vcn.Id,
                CidrBlock = subnetCidrBlock,
                DisplayName = "temasek3-prod-subnet",
            }
        );

        var instance = new Instance(
            "temasek3-prod",
            new InstanceArgs
            {
                CompartmentId = compartmentOcid,
                AvailabilityDomain = "YAbW:AP-SINGAPORE-1-AD-1",
                DisplayName = "temasek3-prod",
                Shape = "VM.Standard.A1.Flex",

                ShapeConfig = new Pulumi.Oci.Core.Inputs.InstanceShapeConfigArgs
                {
                    Ocpus = 1,
                    MemoryInGbs = 2,
                },

                CreateVnicDetails = new Pulumi.Oci.Core.Inputs.InstanceCreateVnicDetailsArgs
                {
                    SubnetId = subnet.Id,
                },

                SourceDetails = new Pulumi.Oci.Core.Inputs.InstanceSourceDetailsArgs
                {
                    SourceType = "image",
                    SourceId = ubuntu24ImageOcid,
                    BootVolumeSizeInGbs = "50",
                },

                FreeformTags = { { "managed-by", "pulumi" } },
            }
        );
    }
}
